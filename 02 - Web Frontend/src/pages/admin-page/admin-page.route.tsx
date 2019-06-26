import * as React from "react";
import { Route } from "react-router";
import { AdminPageContainer } from "./admin-page.container";

export const adminPath = "/admin";

export const AdminRoute = (
  <Route path={adminPath} component={AdminPageContainer} />
);
