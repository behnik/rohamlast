const express = require("express");
const cors = require("cors");
const { rateLimit } = require("express-rate-limit");
const fileUpload = require("express-fileupload");
const http = require("http");

require('dotenv').config();

var app = express();

app.use(cors());

process.env.LOCK_STATUS = { IsExist: false };

app.use(express.static("public"));
app.use(express.json({ limit: "50mb" }));
app.use(express.urlencoded({ limit: "50mb" ,extended: true }));

app.use(fileUpload());

app.set('trust proxy', 1)

const limiter = rateLimit({
    windowMs: 1 * 60 * 1000, // 1 minutes
    limit: 1000, // Limit each IP to 1000 requests per `window` (here, per 1 minutes).
    standardHeaders: 'draft-7', // draft-6: `RateLimit-*` headers; draft-7: combined `RateLimit` header
    legacyHeaders: false, // Disable the `X-RateLimit-*` headers.
    message: async (req, res) => {
        return 'You can only make 1000 requests every hour.'
    },
})

app.use(limiter);

const server = http.createServer(app);

const BASE_PORT = process.env.BASE_PORT;// || 3000;
//const BASE_HOST = process.env.BASE_HOST;// || 3000;

//console.log(BASE_PORT);
if (BASE_PORT === undefined){// || BASE_HOST === undefined) {
    console.log('choose correct BASE_PORT;');
}
else {
    const routes = require('./services/routes');

    const socket_service = require('./services/socket');

    const network = require('./services/network');

    const computers_cron = require('./crons/computers');

    //const ldap_cron = require('./crons/ldap');
    const files_cron = require('./crons/files');

    async function init(addr) {
        await socket_service.init(server, app);

        //await ldap_cron.init();
        await files_cron.init();

        await computers_cron.init();

        await routes.init(app);
        console.log(`server started at http://${addr}:${BASE_PORT};`);
    };

    async function serve() {
         network.get_current_ip((addr) => {
             server.listen(BASE_PORT, addr, async () => { await init(addr); });
         });
    }

    serve();
}