const node_cron = require('node-cron');
const find = require('local-devices');
const axios = require('axios');
const db = require('../services/db');

exports.init = async () => {
    node_cron.schedule('*/30 * * * * *', async () => {
        var devices = await find();
        //console.log(devices);

        devices.forEach(async (device) => {
            try {
                var device_ip = device.ip;
                var computer_name = device.name === '?' ? null : device.name;
                var has_agent = false;

                try {
                    var result = await axios.get(
                        `http://${device_ip}:8900/api/health-check`
                    );

                    console.log(result);

                    if (result.data.code === 200) {
                        computer_name = result.data.data;
                        has_agent = true;
                    }
                } catch (e) {
                    console.log('agent not installed!');
                }

                await db.exec_query(
                    `call prc_get_computer(
                            :ip,
                            :host_name,
                            :has_agent,
                            :fqdn,
                            :platform,
                            :distro,
                            :release,
                            :resources_info);`,
                    {
                        ip: device_ip,
                        host_name: computer_name,
                        has_agent: has_agent,
                        fqdn: null,
                        platform: null,
                        distro: null,
                        release: null,
                        resources_info: null
                    });
            } catch (e) {
                console.log(e);
            }
        });
    });
};