module.exports = {
  assetPrefix: `/memento.core`,
  siteMetadata: {
    title: "memento.core Documents",
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
