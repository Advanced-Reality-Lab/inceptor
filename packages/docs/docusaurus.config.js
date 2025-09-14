/** @type {import('@docusaurus/types').DocusaurusConfig} */
module.exports = {
  title: 'Inceptor Framework',
  tagline: 'A framework for creating interactive, character-driven experiences.',
  url: 'https://Advanced-Reality-Lab.github.io', // Your GitHub Pages URL
  baseUrl: '/inceptor/',                     // The name of your repository
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.ico',
  organizationName: 'Advanced-Reality-Lab',     // Your GitHub username
  projectName: 'inceptor',          // Your repository name

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          // Please change this to your repo.
          editUrl: 'https://github.com/Advanced-Reality-Lab/inceptor/edit/main/packages/docs/',
        },
        blog: false, // We are disabling the blog for now.
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      navbar: {
        title: 'Inceptor Framework',
        logo: {
          alt: 'Inceptor Logo',
          src: 'img/logo.svg',
        },
        items: [
          {
            type: 'doc',
            docId: 'introduction', // THIS IS THE FIX: Changed from 'intro' to 'introduction'
            position: 'left',
            label: 'Documentation',
          },
          // You can add a link to your Clip Explorer app here later
          // {to: 'https://app.useinceptor.org', label: 'Clip Explorer', position: 'left'},
          {
            href: 'https://github.com/Advanced-Reality-Lab/inceptor',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          // ... Footer links can be configured here ...
        ],
        copyright: `Copyright © ${new Date().getFullYear()} Your Name/Lab. Built with Docusaurus.`,
      },
    }),
};
