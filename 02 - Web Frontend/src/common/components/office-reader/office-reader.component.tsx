import * as React from "react";
import { cnc } from "../../../util";

const style = require("./office-reader.style.scss");

interface OfficeReaderProps {
  targetPath: string;
}

interface OfficeReaderState {
  path: any[];
}

export class OfficeReaderComponent extends React.PureComponent<
  OfficeReaderProps,
  OfficeReaderState
> {
  constructor(props) {
    super(props);

    this.state = {
      path: []
    };
  }

  getOfficePreviewPath = (path: string) => {
    return (
      "https://view.officeapps.live.com/op/view.aspx?src=" +
      encodeURIComponent(path)
    );
  };
  public render() {
    return (
      <div className={style.officePreview}>
        <iframe
          src={this.getOfficePreviewPath(this.props.targetPath)}
          className={style.officePreview}
        />
      </div>
    );
  }
}
