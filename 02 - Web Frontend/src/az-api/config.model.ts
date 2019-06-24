/**
 * Object that represents API conection parameters, set and forget parameters
 * or those parameters that do not change frequently.
 */

export interface AzConfig {
  protocol: string;
  serviceUrl: string;
  method: "GET" | "POST";
}

export const defaultAzConfig: AzConfig = {
  protocol: "https",
  serviceUrl: "",
  method: "GET"
};
