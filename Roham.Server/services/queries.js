const db = require('./db');
const permissions_service = require('./permissions');
const users_service = require('./users');
const lock_service = require('./lock');

exports.exec_query = async (app_url, query_url, current_user, query_params) => {
    try {
        var query_response = await this.get_by_query_url(app_url, query_url);
        if (query_response !== null && query_response.length === 1) {
            var query = query_response[0];
            var query_content = query.content;
            var app_id = query.app_id;
            query_content = query_content.replaceAll(':app_id', app_id);
            query_content = query_content.replaceAll(':app_url', app_url);
            if (query.is_public === 0) {
                if (current_user !== null) {
                    var permission_check = 0;

                    var permission_response = await permissions_service.check(current_user.role_id, query_url);

                    if (permission_response !== null && permission_response.length > 0) permission_check = true;

                    if (!permission_check) return { code: 403, message: 'اجازه دسترسی به این آدرس را ندارید!' };
                    else {
                        query_content = query_content.replaceAll(':current_user_id', current_user.id);
                        query_content = query_content.replaceAll(':current_role_id', current_user.role_id);
                        query_content = query_content.replaceAll(':current_user_name', current_user.user_name);

                        if (query.type === 0) {

                            var executed_query = await db.exec_query(query_content, query_params);

                            if (query.is_query === 0) return { code: 200, data: executed_query[0] };
                            else {
                                if (query.is_multiple_result === 1) {
                                    var data = executed_query[0];
                                    var count = executed_query[1][0].count;
                                    return {
                                        code: 200,
                                        data: data,
                                        count: count
                                    };
                                }
                                else return { code: 200, data: executed_query };
                            }
                        }
                        else {
                            //console.log(`query type is ${query.type}`);
                            await eval(query_content);
                            var result = await execute({
                                current_user,
                                query_params,
                                app_url
                            });

                            //console.log(result);
                            return result;
                        }
                    }
                }
                else return { code: 401, message: 'ابتدا باید وارد سیستم شوید!' };
            }
            else {
                if (query.type === 0)
                    return { code: 200, data: await db.exec_query(query_content, query_params) };
                else {
                    await eval(query_content);
                    var result = await execute({
                        current_user,
                        query_params,
                        app_url
                    });
                    return result;
                }
            }
        }
        else return { code: 404, message: 'آدرس یافت نشد!' };
    }
    catch (e) {
        console.log(e);
        var message = '';
        if (e.code === 'ER_ROW_IS_REFERENCED_2') {
            message = 'امکان حذف این رکورد وجود ندارد!';
        }
        else if (e.code === 'ER_DUP_ENTRY') {
            message = 'داده تکراری است!';
        }
        else {
            message = 'خطا در اجرای تابع!';
        }
        return {
            code: 500,
            message: message,
            error: e.code !== undefined ? e.code : JSON.stringify(e)
        };
    }
};

exports.get_by_query_url = async (app_url, query_url) => {
    return await db.exec_query(
        `select q.id,q.content,q.is_public,q.is_multiple_result,q.is_query,q.type,
            a.title,a.id as app_id from queries q
            inner join apps a on a.id = q.app_id
            where a.url = :app_url and q.url=:query_url`,
        {
            app_url: app_url,
            query_url: query_url
        }
    );
};

exports.init_routes = async (app) => {
    app.post('/api/:app_url/:query_url',
        [lock_service.check, users_service.check_authenticate],
        async (req, res) => {
            var app_url = req.params.app_url;
            var query_url = req.params.query_url;
            var query_params = req.body;
            var current_user = req.user;

            var response = await this.exec_query(app_url, query_url, current_user, query_params);

            await db.add_api_log({
                url: `${app_url}/${query_url}`,
                request: query_params,
                response: response,
                user : current_user !== null ? current_user.user_name : null
            });

            res.send(response);
        });
};