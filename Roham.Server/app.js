const express = require("express");
const cors = require("cors");
const { rateLimit } = require("express-rate-limit");

require('dotenv').config();

const http = require("http");

var app = express();

//app.set('lock_check', { exist: false });

app.use(cors());
const fileUpload = require("express-fileupload");

app.use(express.static("public"));
app.use(express.json({ limit: "50mb" }));
app.use(express.urlencoded({ limit: "50mb" }));

app.use(fileUpload());

const limiter = rateLimit({
    windowMs: 1 * 60 * 1000, // 1 minutes
    limit: 1000, // Limit each IP to 1000 requests per `window` (here, per 1 minutes).
    standardHeaders: 'draft-7', // draft-6: `RateLimit-*` headers; draft-7: combined `RateLimit` header
    legacyHeaders: false, // Disable the `X-RateLimit-*` headers.
    message: async (req, res) => {
        return 'You can only make 1200 requests every hour.'
    },
})

app.use(limiter);

const server = http.createServer(app);

const BASE_PORT = process.env.BASE_PORT;// || 3000;
const BASE_HOST = process.env.BASE_HOST;// || 3000;

if (BASE_PORT === undefined){// || BASE_HOST === undefined) {
    console.log('choose correct BASE_PORT or BASE_HOST;');
}
else {
    const routes = require('./services/routes');

    const socket_service = require('./services/socket');

    const network = require('./services/network');

    //const ldap_cron = require('./crons/ldap');
    const files_cron = require('./crons/files');

    async function init(addr) {
        await socket_service.init(server, app);

        //await ldap_cron.init();
        await files_cron.init();

        await routes.init(app);
        console.log(`server started at http://${addr}:${BASE_PORT};`);
    };

    async function serve() {
         network.get_current_ip((addr) => {
             server.listen(BASE_PORT, addr, async () => { await init(addr); });
         });

        //server.listen(BASE_PORT,
        //    BASE_HOST,
        //    async () => { await init(); });
    }

    serve();
}