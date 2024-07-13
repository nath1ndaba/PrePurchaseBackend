"use strict";

const { args, build } = require("./utils");
const { exec } = require("child_process");
const colors = require("colors/safe");

const { build_type, use_cache, isBuild, isApp, version, heroku_tag } = args;

let suffix = "preview";
if (build_type.startsWith("prod")) {
  suffix = "stable";
} else if (build_type.startsWith("dev")) {
  suffix = "dev";
}

if (isBuild) {
  build(exportVariables, suffix);
} else {
  start();
}

const tagged_image = `${heroku_tag}:v${version}-${suffix}`;
const tag = `${heroku_tag}:latest`;

function exportVariables() {
  let command =
    `echo ::set-output name=tagged_image::${tagged_image} && ` +
    `echo ::set-output name=tag::${tag}`;

  console.log(colors.green(command));

  const commandProcess = exec(command);

  commandProcess.stdout.on("data", (data) =>
    console.log(colors.green(data.toString()))
  );

  commandProcess.stderr.on("data", (data) =>
    console.log(colors.cyan(`BUILD OUTPUT: ${data.toString()}`))
  );
}

function run() {
  let c_s = use_cache ? "" : "--no-cache";
  let command = `build ${c_s} --force-rm`;
  if (isApp) {
    command = `docker ${command} -t ${tagged_image} -t ${tag} `;
    command += `-f ./Web_Api/Dockerfile.stella ./Web_Api --build-arg NETCORESDK_VERSION=3.1-buster --build-arg ASPNETCORE_VERSION=3.1-buster-slim`;
  } else {
    command = `docker-compose ${command}`;
  }

  console.log(colors.green(command));

  const commandProcess = exec(command);

  commandProcess.stdout.on("data", (data) =>
    console.log(colors.green(data.toString()))
  );

  commandProcess.stderr.on("data", (data) =>
    console.log(colors.cyan(`BUILD OUTPUT: ${data.toString()}`))
  );

  commandProcess.on("close", (code) => {
    if (code != 0) return console.log(colors.red("ERROR with the build!"));

    console.log(colors.blue("build successful\n"));
    if (!isApp) {
      start();
    } else {
      // tag the image to heroku tag for thr heroku registry

      let cmd = `docker tag ${tag} registry.heroku.com/${heroku_tag}/web`;

      console.log(cmd);

      exec(cmd, (err, stdout, stderr) => {
        if (err) {
          console.log(colors.red("ERROR: " + err));
          return console.log(colors.red("ERROR: " + stderr));
        }

        console.log(colors.cyan(stdout));
        console.log(colors.blue("tagged image"));
      });
    }
  });
}

function start() {
  const startProcess = exec(`docker-compose up -d`);

  startProcess.stderr.on("data", (data) =>
    console.log(colors.yellow(`WARNING : ${data.toString()}`))
  );
  startProcess.stdout.on("data", (data) =>
    console.log(colors.green(data.toString()))
  );

  startProcess.on("error", (err) => console.log(colors.red(`ERROR: ${err}`)));
}
