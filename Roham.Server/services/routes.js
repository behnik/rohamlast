const users_service = require('./users');
const files_service = require('./files');
const queries_service = require('./queries');
const pages_service = require('./pages');
const widgets_service = require('./widgets');
const lock_service = require('./lock');
const report_service = require('./reports');

exports.init = async (app) => {

    app.get('/',
        [lock_service.check],
        (req, res) => {
            res.redirect('/pages/admin');
        });
        
    await users_service.init_routes(app);    

    await files_service.init_routes(app);
    
    await queries_service.init_routes(app);
    
    await pages_service.init_routes(app);

    await widgets_service.init_routes(app);
    
    await report_service.init_routes(app);
};