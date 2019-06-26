import * as React from "react";
import { withRouter, RouteComponentProps } from "react-router";
import { UploadPageComponent } from "./upload-page.component";
import { searchPath } from "../search-page";
import { parseConfig } from "./config.parser";
import { azServiceConfig } from "../search-page/service/az";

class UploadPageInnerContainer extends React.Component<
  RouteComponentProps<any>
> {
  constructor(props) {
    super(props);
  }

  private handleClose = () => {
    this.props.history.push(searchPath);
  };

  public render() {
    return (
      <UploadPageComponent
        onCloseClick={this.handleClose}
        uploadEndpoint={parseConfig(azServiceConfig.uploadConfig)}
      />
    );
  }
}

export const UploadPageContainer = withRouter(UploadPageInnerContainer);
