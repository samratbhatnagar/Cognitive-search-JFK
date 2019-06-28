import { DetailRouteState } from ".";

let detailState: DetailRouteState = {
  hocr: "",
  targetWords: [],
  type: "",
  targetPath: ""
};

export const setDetailState = (state: DetailRouteState) => {
  detailState = state;
};

export const getDetailState = (): DetailRouteState => {
  return detailState;
};
