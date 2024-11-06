const db = require('./db');
const lock_service = require('./lock');

exports.get_by_name = async (report_name) => {
    try {
        return await db.exec_query('select * from reports where name = :report_name;', {
            report_name: report_name
        });
    } catch (e) {
        return null;
    }
};

exports.init_routes = async(app)=>{
    // app.post('/api/reports',
    //     [lock_service.check],
    //     async (req, res) => {
    //         var report_name = req.body.report_name;
    //         var report_data = req.body.report_data;

    //         var report = await this.get_by_name(report_name);

    //         var html = `<html>
    //                     <head>
                            
    //                         <script src="/js/stimulsoft.reports.js"></script>

    //                         <script src="/js/stimulsoft.viewer.js"></script>
    //                         /*<script>
    //                             //Activation with using license code

    //                             //Stimulsoft.Base.StiLicense.Key = "6vJhGtLLLz2GNviWmUTrhSqnOItdDwjBylQzQcAOiHkgpgFGkUl79uxVs8X+uspx6K+tqdtOB5G1S6PFPRrlVNvMUiSiNYl724EZbrUAWwAYHlGLRbvxMviMExTh2l9xZJ2xc4K1z3ZVudRpQpuDdFq+fe0wKXSKlB6okl0hUd2ikQHfyzsAN8fJltqvGRa5LI8BFkA/f7tffwK6jzW5xYYhHxQpU3hy4fmKo/BSg6yKAoUq3yMZTG6tWeKnWcI6ftCDxEHd30EjMISNn1LCdLN0/4YmedTjM7x+0dMiI2Qif/yI+y8gmdbostOE8S2ZjrpKsgxVv2AAZPdzHEkzYSzx81RHDzZBhKRZc5mwWAmXsWBFRQol9PdSQ8BZYLqvJ4Jzrcrext+t1ZD7HE1RZPLPAqErO9eo+7Zn9Cvu5O73+b9dxhE2sRyAv9Tl1lV2WqMezWRsO55Q3LntawkPq0HvBkd9f8uVuq9zk7VKegetCDLb0wszBAs1mjWzN+ACVHiPVKIk94/QlCkj31dWCg8YTrT5btsKcLibxog7pv1+2e4yocZKWsposmcJbgG0";

    //                             // Creating a new report object
    //                             var report = Stimulsoft.Report.StiReport.createNewReport();

    //                             var dataSet = new Stimulsoft.System.Data.DataSet("SimpleDataSet");

    //                             dataSet.readJsonFile("/stimulsoft-reports-js/tikeckt.json");

    //                             report.regData(dataSet.dataSetName, "", dataSet);

    //                             report.dictionary.synchronize();

    //                             // Loading a report template (MRT) into the report object
    //                             report.loadFile("/stimulsoft-reports-js/Report.mrt");

    //                             var viewer = new Stimulsoft.Viewer.StiViewer();

    //                             viewer.report = report;
    //                         </script>*/
    //                         <script type="text/javascript">
    //                             // Create the report viewer with default options
    //                             var viewer = new Stimulsoft.Viewer.StiViewer(null, "StiViewer", false);
    //                             // Create a new report instance
    //                             var report = new Stimulsoft.Report.StiReport();
    //                             // Load report from url
    //                             report.loadFile("/js/Report.mrt");
    //                             // Assign report to the viewer, the report will be built automatically after rendering the viewer
    //                             viewer.report = report;
    //                         </script>
    //                     </head>
    //                     <body></body>
    //                     </html>
    //                     `;

    //         res.send(html);
    //     });
};