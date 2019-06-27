import * as React from "react";
import { cnc } from "../../../util";

const style = require("./global-toolbar.style.scss");

interface Props {
  onSearchClick: () => void;
  onAdminClick: () => void;
}

interface State {}

export class GlobalToolbarComponent extends React.PureComponent<Props, State> {
  constructor(props) {
    super(props);
  }
  render() {
    return (
      <div className={cnc(style.gloablToolbar)}>
        <button onClick={this.props.onSearchClick}>Search</button>
        <button onClick={this.props.onAdminClick}>Admin</button>
      </div>
    );
  }
}
