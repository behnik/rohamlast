const db = require('./db');
const ejs = require('ejs');
const lock_service = require('./lock');

exports.render_page = async (app_url, page_url, page_param) => {
    try {
        var page_response = await this.get_by_page_url(app_url, page_url);

        if (page_response !== null && page_response.length === 1) {
            var page = page_response[0];
            var page_type = page.type;
            var layout_content = '';

            if (page.layout_content !== null) {
                layout_content = page.layout_content.replace('<page-title></page-title>', `<title>${page.title}</title>`);
                layout_content = layout_content.replace('<page-body></page-body>', page.page_content);
            }
            else layout_content = page.page_content;

            var html = ejs.render(layout_content, {
                page_title: page.title,
                page_param: page_param,
                page_data: null,
                app_url: page.app_url,
                app_id: page.app_id,
                app_title: page.app_title
            });

            var content_type = '';
            switch (page_type) {
                case 1:
                    content_type = 'text/html';
                    break;
                case 2:
                    content_type = 'application/javascript';
                    break;
                case 3:
                    content_type = 'text/css';
                    break;
                case 4:
                    content_type = 'application/json';
                    break;
            }
            return {
                code: 200,
                data: html,
                content_type
            };
        }
        else return { code: 404, message: 'صفحه ای با این مشخصات یافت نشد!' };
    } catch (e) {
        console.log(e);
        return { code: 500, message: 'خطا در بارگذاری صفحه!', error: JSON.stringify(e) };
    }
};

exports.get_by_page_url = async (app_url, page_url) => {
    return await db.exec_query(`select p.id,p.url,p.title,p.type,p.content as page_content,
                                l.content as layout_content,
                                a.url as app_url,a.id as app_id,a.title as app_title
                    from pages p 
					inner join apps a on a.id = p.app_id
                    left join layouts l on l.id = p.layout_id
                    where a.url = :app_url and p.url = :page_url`,
        {
            app_url: app_url,
            page_url: page_url
        });
};

exports.init_routes = async(app)=>{
    app.get('/pages/:app_url/:page_url?/:page_param?',
        [lock_service.check],
        async (req, res) => {

            var app_url = req.params.app_url;
            var page_url = req.params.page_url;
            var page_param = req.params.page_param;

            if (page_url === null || page_url === undefined) page_url = 'main';

            var page_response = await this.render_page(app_url, page_url, page_param);

            if (page_response.code === 200) {
                res.set('Content-Type', page_response.content_type);
                res.send(page_response.data);
            }
            else res.status(page_response.code).send(page_response.message);
        });
};