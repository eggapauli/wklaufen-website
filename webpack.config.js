var path = require("path");
var webpack = require("webpack");
var MinifyPlugin = require("terser-webpack-plugin");

function resolve(filePath) {
    return path.join(__dirname, filePath)
}

var CONFIG = {
    fsharpEntry: {
        "app": [
            // "whatwg-fetch",
            // "@babel/polyfill",
            resolve("./src/WkLaufen.Website/App.fs.js")
        ]
    },
    devServerProxy: {
        '/*.php': {
            target: 'http://localhost:8081',
            changeOrigin: true
        }
    },
    historyApiFallback: {
        index: resolve("./index.html")
    },
    contentBase: resolve("./public"),
    // Use babel-preset-env to generate JS compatible with most-used browsers.
    // More info at https://github.com/babel/babel/blob/master/packages/babel-preset-env/README.md
    babel: {
        presets: [
            ["@babel/preset-env", {
                "targets": {
                    "browsers": ["last 2 versions"]
                },
                "modules": false,
                "useBuiltIns": "usage",
                "corejs": "3"
            }]
        ],
        plugins: ["@babel/plugin-transform-runtime"]
    }
};

var isProduction = process.argv.indexOf("serve") == -1;
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

module.exports = {
    entry: CONFIG.fsharpEntry,
    output: {
        path: resolve("./public/js"),
        publicPath: "/js",
        filename: "[name].js",
    },
    mode: isProduction ? "production" : "development",
    devtool: isProduction ? undefined : "source-map",
    resolve: {
        symlinks: false
    },
    optimization: {
        // Split the code coming from npm packages into a different file.
        // 3rd party dependencies change less often, let the browser cache them.
        splitChunks: {
            cacheGroups: {
                commons: {
                    test: /node_modules/,
                    name: "vendors",
                    chunks: "all"
                }
            }
        },
        minimizer: isProduction ? [new MinifyPlugin()] : [],
        moduleIds: isProduction ? "deterministic" : "named"
    },
    // DEVELOPMENT
    //      - HotModuleReplacementPlugin: Enables hot reloading when code changes without refreshing
    plugins: [
        ...(isProduction ? [] : [ new webpack.HotModuleReplacementPlugin() ])
    ],
    // Configuration for webpack-dev-server
    devServer: {
        proxy: CONFIG.devServerProxy,
        hot: true,
        inline: true,
        historyApiFallback: CONFIG.historyApiFallback,
        contentBase: CONFIG.contentBase
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: CONFIG.babel
                },
            },
            {
                test: /\.s(a|c)ss$/,
                use: [
                    "style-loader",
                    "css-loader",
                    "sass-loader"
                ]
            },
            {
                test: /\.css$/,
                use: [
                    "style-loader",
                    "css-loader"
                ]
            },
            {
                test: /\.(eot|svg|ttf|woff|woff2)(\?|$)/,
                use: {
                    loader: 'url-loader',
                    options: {
                        name: '[path][name].[ext]'
                    }
                }
            },
            {
                test: /\.gif$/,
                use: {
                    loader: 'url-loader',
                    options: {
                        name: '[path][name].[ext]'
                    }
                }
            }
        ]
    }
};
