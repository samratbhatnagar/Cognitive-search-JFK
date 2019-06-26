import { defaultAzPayload, defaultAzResponseConfig } from "../../../../az-api";
import { ServiceConfig } from "..";
import {
  mapStateToSuggestionPayload,
  mapSuggestionResponseToState
} from "./mapper.suggestion";
import {
  mapStateToSearchPayload,
  mapSearchResponseToState
} from "./mapper.search";

export const azServiceConfig: ServiceConfig = {
  serviceId: "jfk-docs",
  serviceName: "JFK Documents",
  serviceIcon: "fingerprint",

  searchConfig: {
    apiConfig: {
      protocol: process.env.SEARCH_CONFIG_PROTOCOL,
      serviceUrl: process.env.SEARCH_CONFIG_SERVICE_URL,
      method: "POST"
    },
    defaultPayload: defaultAzPayload,
    responseConfig: defaultAzResponseConfig,
    mapStateToPayload: mapStateToSearchPayload,
    mapResponseToState: mapSearchResponseToState
  },

  suggestionConfig: {
    apiConfig: {
      protocol: process.env.SUGGESTION_CONFIG_PROTOCOL,
      serviceUrl: process.env.SUGGESTION_CONFIG_SERVICE_URL,
      method: "GET"
    },
    defaultPayload: {
      ...defaultAzPayload,
      count: false,
      top: 15,
      suggesterName: "sg-jfk"
      //autocompleteMode: "twoTerms",
    },
    mapStateToPayload: mapStateToSuggestionPayload,
    mapResponseToState: mapSuggestionResponseToState
  },

  graphConfig: {
    protocol: process.env.FUNCTION_CONFIG_PROTOCOL,
    serviceUrl: process.env.FUNCTION_CONFIG_SERVICE_URL,
    method: "POST"
  },
  uploadConfig: {
    protocol: process.env.UPLOAD_CONFIG_PROTOCOL,
    serviceUrl: process.env.UPLOAD_CONFIG_SERVICE_URL
  },
  initialState: {
    facetCollection: [
      {
        fieldId: "entities",
        displayName: "Entities",
        iconName: null,
        selectionControl: "checkboxList",
        values: null,
        config: {
          count: 20
        }
      }
    ]
  }
};
