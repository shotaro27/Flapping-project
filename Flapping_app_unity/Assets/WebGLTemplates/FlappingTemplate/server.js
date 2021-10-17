const { BlobServiceClient } = require('@azure/storage-blob');
const { v1: uuidv1} = require('uuid');

async function saveData(dat) {
	const AZURE_STORAGE_CONNECTION_STRING = "DefaultEndpointsProtocol=https;AccountName=flapping;AccountKey=sEhkXjzMVQZXDdf0ssWPnyEW/jjUplivYJtSpKeUDhOi6YkPMU3g6RJ/NhYvkynp9uR/DM+IXlr+OlE6/46rJQ==;EndpointSuffix=core.windows.net";
	const blobServiceClient = BlobServiceClient.fromConnectionString(AZURE_STORAGE_CONNECTION_STRING);

	const containerName = "flaps";
	const containerClient = blobServiceClient.getContainerClient(containerName);

	const f = uuidv1();
	const blobName = `${f}.json`;
	const blockBlobClient = containerClient.getBlockBlobClient(blobName);

	const data = JSON.stringify(dat);
	await blockBlobClient.upload(data, data.length);

	// for await (const blob of containerClient.listBlobsFlat()) {
	// 	res.write(`\t${blob.name}\n`);
	// }

	// const downloadBlockBlobResponse = await blockBlobClient.download(0);
	// const d = await streamToString(downloadBlockBlobResponse.readableStreamBody);
	// const ddata = JSON.parse(d).name;
	// res.write(`\n\t${ddata}\n`);

	// async function streamToString(readableStream) {
	// 	return new Promise((resolve, reject) => {
	// 		const chunks = [];
	// 		readableStream.on("data", (data) => {
	// 			chunks.push(data.toString());
	// 		});
	// 		readableStream.on("end", () => {
	// 			resolve(chunks.join(""));
	// 		});
	// 		readableStream.on("error", reject);
	// 	});
	// }
}

async function getData() {
	const AZURE_STORAGE_CONNECTION_STRING = "DefaultEndpointsProtocol=https;AccountName=flapping;AccountKey=sEhkXjzMVQZXDdf0ssWPnyEW/jjUplivYJtSpKeUDhOi6YkPMU3g6RJ/NhYvkynp9uR/DM+IXlr+OlE6/46rJQ==;EndpointSuffix=core.windows.net";
	const blobServiceClient = BlobServiceClient.fromConnectionString(AZURE_STORAGE_CONNECTION_STRING);

	const containerName = "flaps";
	const containerClient = blobServiceClient.getContainerClient(containerName);

	let f = [];
	for await (const blob of containerClient.listBlobsFlat()) {
		// console.log(`\t${blob.name}\n`);
		const blockBlobClient = containerClient.getBlockBlobClient(blob.name);
		const downloadBlockBlobResponse = await blockBlobClient.download(0);
		const d = await streamToString(downloadBlockBlobResponse.readableStreamBody);
		// const ddata = JSON.parse(d).name;
		// console.log(`\n\t${ddata}\n`);
		f.push(JSON.parse(d));
	}

	async function streamToString(readableStream) {
		return new Promise((resolve, reject) => {
			const chunks = [];
			readableStream.on("data", (data) => {
				chunks.push(data.toString());
			});
			readableStream.on("end", () => {
				resolve(chunks.join(""));
			});
			readableStream.on("error", reject);
		});
	}

	return new Promise((resolve, reject) => {
		resolve(f);
	});
}

var urlParser = require('url'),
	handler = function(req, res) {
		var urlParsed = urlParser.parse(req.url),
			filepath = "";
		console.log(urlParsed.pathname);
		filepath += urlParsed.pathname.toString();
		if (urlParsed.pathname == "/") {
			filepath += "index.html";
		}
		fs.readFile(__dirname + filepath,
			function (err, data) {
				if (err) {
					console.log(err);
					res.writeHead(500);
					return res.end("Error");
				}
				res.writeHead(200);
				res.write(data);
				res.end();
			}
		);
	},
	http = require("http"),
	app = http.createServer(handler),
	fs = require("fs"),
	io = require("socket.io")(app);
	// ws = require ('ws').Server;
app.listen(process.env.PORT || 8000);
var flaps = [];
var positions = [];
var diffs = [];
getData().then((f) => {
	flaps = f;
	flaps.sort((a, b) => a.id - b.id);
	positions = flaps.map(fl => ({id: fl.id, pos: {x: 0, y: 0, z: 0}}));
	diffs = flaps.map(fl => ({id: fl.id, pos: {x: 0, y: 0, z: 0}}));
	console.log(diffs);
}).catch((ex) => console.log(ex.message));
io.on('connection', function (socket) {
	console.log("connected");
	socket.emit("emit_from_server", "connected");
	socket.on("emit_from_client", function (d) {
		var now = new Date();
		var data = {data: d.data, name: d.name, id: flaps.length};
		// console.log(d);
		// console.log (now.toLocaleString() + ' Received: %s', data);
		socket.broadcast.emit("emit_from_server", data);
		saveData(data).then(() => console.log('Done')).catch((ex) => console.log(ex.message));
		flaps.push(data);
		positions.push({id: data.id, pos: {x: 0, y: 0, z: 0}});
		diffs.push({id: data.id, pos: {x: 0, y: 0, z: 0}});
		socket.broadcast.emit("add_to_garden", JSON.stringify(data));
	});
	socket.on("setPos", d => {positions.filter(f => f.id == d.id)[0].pos = d.pos; console.log(d.pos);});
	socket.on("setDiff", d => {diffs.filter(f => f.id == d.id)[0].pos = d.pos; console.log(d.pos);});
	socket.on("sendFlapDatas", function (d) {
		console.log("send");
		for (const p of positions.slice(-15)) {
			console.log(p.pos);
			io.to(d.id).emit("pos_set", JSON.stringify(p));
		}
		for (const df of diffs.slice(-15)) {
			io.to(d.id).emit("diff_set", JSON.stringify(df));
		}
		for (const f of flaps.slice(-15)) {
			io.to(d.id).emit("data_set", JSON.stringify(f));
		}
	});
	socket.on("getFlapData", function (d) {
		// if (flaps.some(f => f.id == d.id)) {
		// 	socket.emit("flapData", JSON.stringify(flaps.filter(f => f.id == d.id)[0]));
		// } else {
		// 	socket.emit("flapData", JSON.stringify({data: null, name: null, id: d.id}));
		// }
		for (const f of flaps.slice(d.id)) {
			socket.emit("flapData", JSON.stringify(f));
		}
		if (flaps.length <= d.id) {
			socket.emit("sendFlapData", "");
		}
	});
	socket.on("addFlap", function (d) {
		socket.broadcast.emit("pos_set", JSON.stringify(positions.filter(f => f.id == d.id)[0]));
		socket.broadcast.emit("diff_set", JSON.stringify(diffs.filter(f => f.id == d.id)[0]));
		socket.broadcast.emit("data_set", JSON.stringify(flaps.filter(f => f.id == d.id)[0]));
	});
	socket.on("removeFlap", d => socket.broadcast.emit("remove_flap", d.id));
	socket.on("emit_from_viewer", function () {
		console.log("flap");
		socket.broadcast.emit("accessFromViewer", socket.id);
	});
	for (const f of flaps.slice(-15)) {
		socket.emit("emit_to_garden", JSON.stringify(f));
	}
});