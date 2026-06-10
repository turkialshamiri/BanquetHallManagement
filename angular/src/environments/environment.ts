import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:51666';

const oAuthConfig = {
  issuer: 'https://localhost:44324/',
  redirectUri: baseUrl,
  clientId: 'BanquetHallManagement_App',
  // Use password flow to support an in-app login page.
  responseType: 'password',
  scope: 'offline_access BanquetHallManagement',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'BanquetHallManagement',
  },
  localization: {
    defaultResourceName: 'BanquetHallManagement',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44324',
      rootNamespace: 'BanquetHallManagement',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
