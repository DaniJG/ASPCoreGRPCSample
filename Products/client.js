const fs = require('fs');
let grpc = require("grpc");
var protoLoader = require("@grpc/proto-loader");
 

const SERVER_ADDRESS = "localhost:5004";
 
// Load protobuf
let proto = grpc.loadPackageDefinition(
  protoLoader.loadSync("../Protos/products.proto", {
    keepCase: true,
    longs: String,
    enums: String,
    defaults: true,
    oneofs: true
  })
);
 
// Create client
const credentials = grpc.credentials.createSsl(
  fs.readFileSync('./certs/ca.crt'), 
  fs.readFileSync('./certs/client.key'), 
  fs.readFileSync('./certs/client.crt')
);
// let client = new proto.products.ProductsInventory(
//   SERVER_ADDRESS,
//   grpc.credentials.createInsecure()
// );
let client = new proto.products.ProductsInventory(
  SERVER_ADDRESS,
  credentials
);

client.Details({ productId: "1234" }, (err, details) => {
  console.log('Received details:', details);
});
