"use strict";

const args = require("yargs").argv;
const pkg = require("./package.json");
const fs = require("fs");
const colors = require("colors/safe");

let updateVersion = true;

function _getArgs() {
  const build_type = (args.build || "preview").toLocaleLowerCase();
  const heroku_tag = (args.tag || "dev-stella-web").toLocaleLowerCase();
  const port = args.port || 8000;

  const use_cache =
    Object.keys(args).includes("use-cache") ||
    Object.keys(args).includes("useCache") ||
    false;

  const isApp =
    Object.keys(args).includes("app") ||
    Object.keys(args).includes("a") ||
    false;

  let version = pkg.build_versions[build_type];
  const sVersion = sematicVersion(version);

  return {
    build_type,
    port,
    use_cache,
    version: sVersion.version,
    isBuild: args.build != "undefined" || args.build != "null",
    isApp,
    heroku_tag,
  };
}

const objectArgs = _getArgs();

function writeEnv(tag, port, volume_ps, cb) {
  try {
    // read the current envs
    let data = "";

    if (fs.existsSync(".env")) {
      data = fs.readFileSync(".env", "utf8");
    }
    // split on new lines
    let lines = data.split(/\r?\n/);

    let envs = 0; // a bit flag to detect what envs have been read
    // 0000 - no envs
    // 0001 - E_PORT
    // 0010 - TAG
    // 0100 - VOLUME

    // a loop to look for required envs
    for (let i = 0; i < lines.length; i++) {
      if (envs & 1 && envs & 2 && envs & 4) {
        break; // all required envs ave been set
      }
      if (lines[i].includes("E_PORT=")) {
        lines[i] = `E_PORT=${port}`;
        //1: 0001
        envs = envs | 1;
      } else if (lines[i].includes("TAG=")) {
        lines[i] = `TAG=${tag}`;
        //2: 0010
        envs = envs | 2;
      } else if (lines[i].includes("VOLUME=")) {
        lines[i] = `VOLUME=${volume_ps[0]}`;
        envs = envs | 4;
      }
    }

    // set any envs that were not set

    if (!(envs & 1)) {
      lines[lines.length] = `E_PORT=${port}`;
    }

    if (!(envs & 2)) {
      lines[lines.length] = `TAG=${tag}`;
    }

    data = lines.join("\n"); // join the env array into a string

    // write the envs back to .env file
    fs.writeFile(".env", data, (err) => {
      if (err) return console.log(colors.red(err));

      // callback
      cb();
    });
  } catch (err) {
    console.log(colors.red(err));
  }
}

function sematicVersion(version) {
  version = version.split(".");

  // add missing versions

  for (let i = 0; i < 3; i++) {
    version[i] = version[i] || 0;
    version[i] = Number(version[i]);
  }
  if (updateVersion) {
    // increment the patch version

    version[2] += 1;

    updateVersion = false;
  }

  const major_v = Number(version[0]);
  const minor_v = Number(version[1]);
  const patch_v = Number(version[2]);
  version = `${major_v}.${minor_v}.${patch_v}`;

  return { version, major_v, minor_v, patch_v };
}

function writeFilePromise(file, content) {
  return new Promise((resolve, reject) => {
    fs.writeFile(file, content, (err) => {
      if (err) return resolve({ success: false, error: error });

      resolve({ success: true });
    });
  });
}

async function build(onSuccess, suffix) {
  let { build_type, version, port, volume_ps } = objectArgs;
  pkg.build_versions[build_type] = version;
  version = `v${version}-${suffix}`;

  console.log(colors.white("writing to ./package.json"));
  console.log(version);
  const content = JSON.stringify(pkg, undefined, 2);
  const { success, error } = await writeFilePromise("./package.json", content);

  if (!success) return console.log(colors.red(error));

  console.log(colors.white("writing to .env file"));
  writeEnv(version, port, volume_ps, () => {
    console.log(colors.white("build: "), colors.green(`${version}`));
    console.log(colors.white("port: "), colors.green(`${port}`));
    onSuccess();
  });
}

module.exports = { writeEnv, sematicVersion, args: objectArgs, build };
