/**
 * Object that represents API payload parameters. 
 * So far, it only contains the search term.
 */

export interface GraphPayload {
  search: string;
  facet: string;
}

export const defaultGraphPayload: GraphPayload = {
  search: "",
  facet: "keyPhrases"
}