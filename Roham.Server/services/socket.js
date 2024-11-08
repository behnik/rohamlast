//const computers_service = require('./computers');

exports.init = async (server, app) => {
    const io = require("socket.io")(server);
    io.on("connection", async (socket) => {
        //console.log(socket.id + " a user connected");

        socket.on("disconnect", async () => {
            //console.log("user disconnected");
        });

        //socket.on("hardware", async (data) => {
        //    try {
        //        console.log(`hardware emit called.`);
        //        console.log(data.host_name);
        //        await computers_service.upsert(data);

        //        await io.sockets.emit("computerAdded", data);

        //        return { code: 200 };
        //    } catch (e) {
        //        console.log(e);
        //        return { code: 500, message: 'hardware emit failed.'};
        //    }
        //});

        socket.on("monitoring", async (data) => {
            await io.sockets.emit("monitoring", data);
        });

        socket.on("device_log", async (data) => {
            io.sockets.emit("device_log", data);
        });
    });
};