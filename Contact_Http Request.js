function executeRequest(PrimaryControl) {
    var formContext = PrimaryControl;
    var flowUrl = "https://prod-172.westus.logic.azure.com:443/workflows/6817e694a6784a9bbc82bfb1d2f08226/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=CxWCnrbFepOErZkd40nJiyYtMX8xdtbzMSFj8-D5nq8";
    var input = JSON.stringify({
        "contactid": formContext.data.entity.getId().replace("{", "").replace("}", "")
    });
    var req = new XMLHttpRequest();
    req.open("POST", flowUrl, true);
    req.setRequestHeader('Content-Type', 'application/json');
    req.send(input);
}