/**
 * Object that represents API conection parameters.
 */

export type GraphMethodType = "POST";

export interface GraphConfig {
  protocol: string;
  serviceUrl: string;
  method: GraphMethodType;
}
