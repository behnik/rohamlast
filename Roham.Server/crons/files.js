const node_cron = require('node-cron');
const file_service = require('../services/files');

exports.init = async () => {
    node_cron.schedule(`*/100 * * * *`, async () => {
        await file_service.delete_old_files();
    });
};