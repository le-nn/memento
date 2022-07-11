module.exports = {
  assetPrefix: `/memento.js`,
  siteMetadata: {
    title: "memento.js Documents",
  },
  plugins: [
    "gatsby-plugin-emotion",
    {
      resolve: "gatsby-source-filesystem",
      options: {
        name: "pages",
        path: "./src/pages/",
      },
      __key: "pages",
    },
  ],
};
