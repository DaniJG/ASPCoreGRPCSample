const fs = require('fs');
const grpc = require('grpc');
const protoLoader = require('@grpc/proto-loader');
const SERVER_ADDRESS = process.env['SERVER_ADDRESS'] || '0.0.0.0:5004';
 
// Load protobuf
const proto = grpc.loadPackageDefinition(
  protoLoader.loadSync('../Protos/products.proto', {
    keepCase: true,
    longs: String,
    enums: String,
    defaults: true,
    oneofs: true
  })
);
 
// Receive GRPC request
function getdetails(call, callback) {  
  const { productId } = call.request;  
  console.log('Sending details for:', productId);
  callback(null, {productId, name: 'mockName', category: 'mockCategory'});
}
 
// Define server with the methods and start it
const server = new grpc.Server();
server.addService(proto.products.ProductsInventory.service, { Details: getdetails });
 
const credentials = grpc.ServerCredentials.createSsl(
  fs.readFileSync('./certs/ca.crt'), 
  [{
    cert_chain: fs.readFileSync('./certs/server.crt'),
    private_key: fs.readFileSync('./certs/server.key')
  }], 
  /*checkClientCertificate*/ false); // The HTTPClient based .NET client cant supply these, only the GRPC.Core client can
// server.bind(SERVER_ADDRESS, grpc.ServerCredentials.createInsecure());
server.bind(SERVER_ADDRESS, credentials);
 
server.start();