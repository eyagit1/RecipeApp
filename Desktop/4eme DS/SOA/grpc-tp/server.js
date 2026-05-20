'use strict';
const path = require('node:path');
const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const PROTO_PATH = path.join(__dirname, 'hello.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
keepCase: false,
longs: String,
enums: String,
defaults: true,
oneofs: true
});
const helloProto = grpc.loadPackageDefinition(packageDefinition).hello;
function sayHello(call, callback) {
const rawName = call.request?.name ?? '';
const name = String(rawName).trim() || 'inconnu';
callback(null, { message: `Bonjour, ${name} !` });
}
function sayBye(call, callback) {
	const rawName = call.request?.name ?? '';
	const name = String(rawName).trim() || 'inconnu';
	callback(null, { message: `Au revoir, ${name} !` });
}
function main() {
const server = new grpc.Server();
server.addService(helloProto.Greeter.service, {
	sayHello,
	sayBye
});
server.bindAsync(
'0.0.0.0:50051',
grpc.ServerCredentials.createInsecure(),
(err, port) => {
if (err) {
console.error('Error starting gRPC server:', err);
return;
}
console.log(`gRPC Server started on 0.0.0.0:${port}`);
}
);
}
main();