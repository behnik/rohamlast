const mariadb = require('mariadb');

const pool = mariadb.createPool({
    host: process.env.DB_HOST,
    port: process.env.DB_PORT,
    user: process.env.DB_USER,
    password: process.env.DB_PASSWORD,
    database: process.env.DB_DATABASENAME,
    connectionLimit: process.env.DB_CONNECTIONLIMIT,
    queueLimit: process.env.DB_QUEUELIMIT,
    multipleStatements: true,
    namedPlaceholders: true,
    bigNumberStrings: true,
    supportBigNumbers: true,
    bigIntAsNumber: true,
    decimalAsNumber: true,
    bigIntAsNumber: true
});

/**
 * اجرای کوئری
 * @param {any} query_content
 * @param {any} query_params
 * @returns
 */
exports.exec_query = async (query_content, query_params) => {
    var conn = await pool.getConnection();
    var query = await pool.query(query_content, query_params);

    conn.end();

    return query;
};

/**
 * اجرای کوئری تک نتیجه
 * @param {any} query_content
 * @param {any} query_params
 * @returns
 */
exports.exec_single_query = async (query_content, query_params) => {
    var conn = await pool.getConnection();
    var query = await pool.query(query_content, query_params);

    conn.end();

    if (query.length > 0) return query[0];
    else return null;
};

/**
 * 
 * @param {any} query_content 
 * @param {any} query_params 
 * @param {any} call_back 
 */
exports.exec_query_sync = (query_content, query_params, call_back) => {
    pool.getConnection().then((conn) => {
        conn.query(query_content, query_params).then((result, err) => {
            if (conn) conn.end();
            if (call_back) call_back(result);
        });
    });
};

/**
 * لود تنظمات با نام
 * @param {any} config_name 
 * @returns 
 */
exports.get_config = async (config_name) => {
    var config = await this.exec_query(
        "select id,name,value from configs where name = :name",
        { name: config_name },
    );
    return config[0].value;
};

exports.add_api_log = async (log) => {
    return await this.exec_query(
        `insert into api_logs(url,request,response,user,ip)
        values(:url,:request,:response,:user,:ip);`
        , log
    );
};