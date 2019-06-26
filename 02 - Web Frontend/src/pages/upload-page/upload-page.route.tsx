import * as React from "react";
import { Route } from "react-router";
import { UploadPageContainer } from "./upload-page.container";

export const uploadPath = "/upload";

export const UploadRoute = (
  <Route path={uploadPath} component={UploadPageContainer} />
);
