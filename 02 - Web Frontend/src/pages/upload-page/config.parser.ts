import { UploadConfig } from "./config.model";

/**
 * Parsers for Config.
 * A parser will do a transformation from Config object to a connection URL.
 */

export const parseConfig = (config: UploadConfig): string => {
  const root = `${config.protocol}://${config.serviceUrl}`;
  const authCode = `?opt=0&`;
  return root + authCode;
};
