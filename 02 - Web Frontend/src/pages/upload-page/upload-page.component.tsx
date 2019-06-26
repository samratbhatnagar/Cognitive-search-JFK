import * as React from "react";
const superagent = require("superagent");
import Dropzone from "react-dropzone";

const style = require("./upload-page.style.scss");

interface UploadPageProps {
  onCloseClick: () => void;
  uploadEndpoint: string;
}
interface UploadPageState {
  acceptedFiles: Array<any>;
}

export class UploadPageComponent extends React.Component<
  UploadPageProps,
  UploadPageState
> {
  constructor(props) {
    super(props);
    this.state = {
      acceptedFiles: []
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
    return (
      <div className={style.container}>
        <span>Upload files</span>
        <Dropzone
          multiple={false}
          onDrop={acceptedFiles => this.setState({ acceptedFiles })}
        >
          {({ getRootProps, getInputProps }) => (
            <section>
              <div {...getRootProps()}>
                <input name="files" {...getInputProps()} />
                <p>Drag 'n' drop some files here, or click to select files</p>
              </div>
              <aside>
                <h4>Files</h4>
                <ul>{files}</ul>
              </aside>
            </section>
          )}
        </Dropzone>
        <input type="submit" value="Upload" onClick={this.handleUpload} />
      </div>
    );
  }
}
