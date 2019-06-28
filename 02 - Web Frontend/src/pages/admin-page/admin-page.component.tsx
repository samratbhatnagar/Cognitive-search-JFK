import * as React from "react";

const style = require("./admin-page.style.scss");

interface AdminPageProps {
  onCloseClick: () => void;
  uploadEndpoint: string;
}
interface AdminPageState {
  acceptedFiles: Array<any>;
  isHovered: boolean;
}

export class AdminPageComponent extends React.Component<
  AdminPageProps,
  AdminPageState
> {
  constructor(props) {
    super(props);
    this.state = {
      acceptedFiles: [],
      isHovered: false
    };
  }

  public render() {
    return (
      <div>
        <span>Select Theme</span>
        <select>
          <option>Theme1</option>
          <option>Theme2</option>
          <option>Theme3</option>
        </select>
        <label>Logo Image</label>
        <input name="file" type="file" />
      </div>
    );
  }
}
