import * as React from "react";
const superagent = require("superagent");
import Dropzone from "react-dropzone";
import { LogoComponent } from "../../../common/components/logo";
import { Button } from "material-ui";

const style = require("./upload-page.style.scss");

interface UploadPageProps {
  onCloseClick: () => void;
  uploadEndpoint: string;
}
interface UploadPageState {
  acceptedFiles: Array<any>;
  isHovered: boolean;
}

export class UploadPageComponent extends React.Component<
  UploadPageProps,
  UploadPageState
> {
  constructor(props) {
    super(props);
    this.state = {
      acceptedFiles: [],
      isHovered: false
    };
    this.handleUpload = this.handleUpload.bind(this);
  }

  handleUpload() {
    const req = superagent.post(this.props.uploadEndpoint);
    let i = 0;
    this.state.acceptedFiles.forEach(file => {
      req.attach(i++, file);
    });
    req.end();
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

    return (
      <div className={style.container}>
        <h2>Upload files</h2>
        <Dropzone
          multiple={false}
          onDragEnter={() => this.setState({ isHovered: true })}
          onDragLeave={() => this.setState({ isHovered: false })}
          onDrop={acceptedFiles => this.setState({ acceptedFiles })}
        >
          {({ getRootProps, getInputProps }) => (
            <section className={className}>
              <div className={style.box} {...getRootProps()}>
                <input name="files" {...getInputProps()} />
                <p>Drag 'n' drop some files here, or click to select files</p>
              </div>
            </section>
          )}
        </Dropzone>
        <section className={style.files}>
          <h4>Files</h4>
          <ul>{files}</ul>
        </section>
        <Button
          variant="raised"
          size="small"
          color="secondary"
          onClick={this.handleUpload}
        >
          Upload
        </Button>
      </div>
    );
  }
}
