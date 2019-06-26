import * as React from "react";
import { withRouter, RouteComponentProps } from "react-router";
import { AdminPageComponent } from "./admin-page.component";
import { searchPath } from "../search-page";
import { parseConfig } from "./settings/config.parser";
import { azServiceConfig } from "../search-page/service/az";
import { UploadPageContainer } from "./upload-page";

interface AdminInnerPageState {
  openTab: string;
}
class AdminPageInnerContainer extends React.Component<
  RouteComponentProps<any>,
  AdminInnerPageState
> {
  constructor(props) {
    super(props);

    this.state = {
      openTab: "theme"
    };
  }

  private handleClose = () => {
    this.props.history.push(searchPath);
  };

  public render() {
    return (
      <div>
        <div>
          <button
            onClick={() => {
              this.setState({ openTab: "theme" });
            }}
          >
            Theme
          </button>
          <button
            onClick={() => {
              this.setState({ openTab: "upload" });
            }}
          >
            Content Upload
          </button>
        </div>
        {this.state.openTab == "theme" && (
          <AdminPageComponent
            onCloseClick={this.handleClose}
            uploadEndpoint={parseConfig(azServiceConfig.uploadConfig)}
          />
        )}
        {this.state.openTab == "upload" && <UploadPageContainer />}
      </div>
    );
  }
}

export const AdminPageContainer = withRouter(AdminPageInnerContainer);
