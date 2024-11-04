const db = require('./db');

exports.check = async (role_id, query_url) => {
    return await db.exec_query(
        `select rq.*
        from role_queries rq
        inner join queries q on q.id = rq.query_id
        where rq.role_id = :role_id and q.url = :query_url;`,
        {
            role_id,
            query_url
        });
};