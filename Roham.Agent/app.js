const express = require("express");
const cors = require("cors");

const si = require('systeminformation');
const logger = require('./logger');

const hardware_info = require('./hardware_info');
const monitoring = require('./monitoring');

const appDir = process.cwd();
console.log(appDir);
require('dotenv').config({ path: `${appDir}\\.env` });

var app = express();

app.use(cors());

app.use(express.static("public"));
app.use(express.json({ limit: "50mb" }));
app.use(express.urlencoded({ limit: "50mb" }));

app.get('/', (req, res) => {
    res.send('Roham Agent Works Fine.');
});

app.get('/api/health-check', (req, res) => {
    try {
        var osinfo = await si.osInfo();
        res.send({ code: 200, data: osinfo.hostname });
    } catch (e) {

    }
});

app.get('/api/get-system-info', async (req, res) => {
    try {
        var osinfo = await si.osInfo();
        var info = await hardware_info.get();
        res.send({
            code: 200,
            data: {
                host_name: osinfo.hostname,
                fqdn: osinfo.fqdn,
                platform: osinfo.platform,
                distro: osinfo.distro,
                release: osinfo.release,
                resources_info: info
            }
        });
    } catch (e) {
        res.send({ code : 500 , data : e });
    }
});

app.get('/api/get-live-info', async (req, res) => {
    try {
        var info = await monitoring.init();
        res.send({ code: 200, data: info });
    } catch (e) {
        res.send({ code: 500, data: e });
    }
});

var base_port = process.env.BASE_AGENT_PORT;

app.listen(base_port, async () => {
    console.log(`server started at ${base_port}.`);
});