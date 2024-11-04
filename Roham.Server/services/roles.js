const db = require('./db');

exports.get_default_role = async () => {
    return await db.exec_query(
        `select * from roles where is_default = 1;`
    );
};

exports.get_role_queries_by_role_id = async (role_id) => {
    return await db.exec_query(
        `select q.id,q.url,q.title,q.app_id
        from role_queries rq
            inner join queries q on q.id = rq.query_id
        where rq.role_id = :role_id`,
        { role_id: role_id },
    );
};