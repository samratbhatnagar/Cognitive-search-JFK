/**
 * Object that represents API conection parameters.
 */

export type GraphMethodType = "GET";

export interface GraphConfig {
  protocol: string;
  serviceUrl: string;
  method: GraphMethodType;
}
