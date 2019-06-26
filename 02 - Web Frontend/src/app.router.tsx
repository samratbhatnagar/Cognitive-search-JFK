import * as React from "react";
import { HashRouter, Switch, Route } from "react-router-dom";
import { HomeRoute } from "./pages/home-page";
import { SearchRoute } from "./pages/search-page";
import { DetailRoute } from "./pages/detail-page";
import { UploadRoute } from "./pages/admin-page/upload-page";
import { AdminRoute } from "./pages/admin-page";

const defs = require("./theme/_theme-setup.scss");

export class AppRouter extends React.Component {
  getCustomBackgroundImagePath() {
    if (defs.appCustomBackgroundImagePath == "none") {
      return "none";
    } else {
      return "url(" + defs.appCustomBackgroundImagePath + ")";
    }
  }

  public componentDidMount() {
    // We just want to display the background image once all the app is ready
    // if not it just doesn't display.
    document.body.style.backgroundImage = this.getCustomBackgroundImagePath();
  }

  public render() {
    return (
      <HashRouter>
        <Switch>
          {HomeRoute}
          {SearchRoute}
          {DetailRoute}
          {UploadRoute}
          {AdminRoute}
        </Switch>
      </HashRouter>
    );
  }
}
