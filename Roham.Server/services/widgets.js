const db = require('./db');
const queries_service = require('./queries');
const users_service = require('./users');
const lock_service = require('./lock');
const { Liquid } = require('liquidjs');

const engine = new Liquid();

exports.render = async (app_url, widget_name, current_user) => {
    try {
        var widgets = await this.get_by_name(app_url, widget_name);
        if (widgets !== null && widgets.length === 1) {
            var widget_content = widgets[0].widget_content;
            var query_url = widgets[0].query_url;

            var query_result;

            if (query_url !== null && query_url !== undefined) {
                console.log(app_url);
                query_result = await queries_service.exec_query(app_url, query_url, current_user, {});
                console.log(query_result.data);
            }

            return {
                code: 200,
                data: await engine.parseAndRender(widget_content, { data: query_result.data })
            };
        }
        else return { code: 404, message: 'ماژول یافت نشد!' };
    }
    catch (e) {
        //console.log(e);
        return {
            code: 500,
            message: 'خطا در بارگذاری ماژول!',
            error: e
        };
    }
};

exports.get_by_name = async (app_url, widget_name) => {
    return await db.exec_query(
        `select w.id,w.content as widget_content,q.url as query_url
        from widgets w
		inner join apps a on a.id = w.app_id
        left join queries q on q.id = w.query_id
		where w.name = :widget_name;`,
        {
            app_url,
            widget_name
        }
    );
};

exports.init_routes = async(app)=>{
    app.get('/widgets/:app_url/:widget_name',
        [lock_service.check, users_service.check_authenticate],
        async (req, res) => {
            var app_url = req.params.app_url;
            var widget_name = req.params.widget_name;
            var current_user = req.user;
            res.send(await this.render(app_url, widget_name, current_user));
        });
};