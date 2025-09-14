/** @type {import('@docusaurus/types').DocusaurusConfig} */
module.exports = {
  title: 'Inceptor Framework',
  tagline: 'A framework for creating interactive, character-driven experiences.',
  url: 'https://www.useinceptor.org', // Your GitHub Pages URL
  baseUrl: '/',                     // The name of your repository
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
          editUrl: 'https://github.com/Advanced-Reality-Lab/inceptor/edit/main/packages/docs/',
        },
        blog: false, 
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
            docId: 'introduction', 
            position: 'left',
            label: 'Documentation',
          },

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
        copyright: `Copyright © ${new Date().getFullYear()} Advanced Reality Lab @ RUNI. Built with Docusaurus.`,
      },
    }),
};
