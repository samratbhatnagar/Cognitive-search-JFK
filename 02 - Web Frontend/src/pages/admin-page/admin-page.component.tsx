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
    const files = this.state.acceptedFiles.map(file => (
      <li key={file.name}>
        {file.name} - {file.size} bytes
      </li>
    ));

    let className = style.upload;

    if (this.state.isHovered) {
      className += " " + style.hover;
    }

    return <div className={style.container} />;
  }
}
