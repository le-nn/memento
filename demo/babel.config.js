module.exports = {
    "presets": [
        "@babel/preset-env",
        "@babel/preset-react",
        "babel-preset-gatsby"
    ],
    "plugins": [
        ["@babel/plugin-proposal-decorators", { legacy: true }],
        "babel-plugin-transform-typescript-metadata"
    ]
};