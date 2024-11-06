const node_cron = require('node-cron');
const axios = require('axios');

exports.init = async (app) => {
    node_cron.schedule(`*/1 * * * *`, async () => {
        try {
            var roham_services_url = process.env.ROHAM_SERVICES_URL;
            var result = await axios.get(`${roham_services_url}/api/locks/check`);
            console.log(result);

            process.env.LOCK_STATUS = result;
        } catch (e) {
            process.env.LOCK_STATUS = { IsExist: false };
        }        
    });
};