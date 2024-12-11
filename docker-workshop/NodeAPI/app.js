var express = require('express');
var app = express();
app.get('/', function (req, res) {
  res.send('Hello from NodeJS in Docker!');
});
app.listen(3000, function () {
  console.log('NodeAPI from Docker app listening on port 3000!');
});
