const db = require('./db');

exports.upsert = async (computer) => {
    try {
        return await db.exec_query(
            `call prc_get_computer(:host_name,:fqdn,:platform,:distro,:release,:resources_info);`,
            computer
        );
    } catch (e) {
        console.log(JSON.stringify(e));
    }
};