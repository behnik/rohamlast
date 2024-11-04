const db = require('./db');
const users_service = require('./users');
const lock_service = require('./lock');

exports.create_file = async (file) => {
    return await db.exec_query(
        `insert into files(name,type,size,content) values(:name,:type,:size,:content);`
        , {
            name: file.file_name,
            type: file.file_type,
            size: file.file_size,
            content: file.file_content
        });
};

exports.get_by_id = async (file_id) => {
    return await db.exec_query('select * from files where id = :id', { id: file_id });
};

exports.delete_old_files = async () => {
    return await db.exec_query(
        `delete from files where cat = 'exports' and created_at < (SELECT CURRENT_DATE - INTERVAL 1 DAY);`
    );
};

exports.init_routes = async(app)=>{
    app.post('/api/files/upload',
        [lock_service.check, users_service.check_authenticate],
        async (req, res) => {
            var file = req.files.files;
            res.send({
                code: 200,
                data: await this.create_file(
                    {
                        file_name: file.name,
                        file_type: file.mimetype,
                        file_size: file.size,
                        file_content: file.data
                    })
            });
        });

    app.get('/api/files/:file_id',
        [lock_service.check],
        async (req, res) => {
            var file_id = req.params.file_id;

            var files = await this.get_by_id(file_id);

            if (files !== null && files.length > 0) {
                var file = files[0].content;

                res.setHeader('Content-Length', file.length);
                res.write(file, 'binary');
                res.end();
            }
            else res.status(404).send('فایلی با این مشخصات یافت نشد!');
        });
};