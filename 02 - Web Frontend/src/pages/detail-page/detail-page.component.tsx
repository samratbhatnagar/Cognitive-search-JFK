import * as React from "react";
import {
  HocrProofreaderComponent,
  PageIndex
} from "../../common/components/hocr";
import { ZoomMode } from "../../common/components/hocr";
import { ToolbarComponent } from "./components/toolbar";
import { HorizontalSeparator } from "../../common/components/horizontal-separator";
import { JsonReaderComponent } from "../../common/components/json-reader";
import { OfficeReaderComponent } from "../../common/components/office-reader";

const style = require("./detail-page.style.scss");

interface DetailPageProps {
  hocr: string;
  targetWords: string[];
  zoomMode?: ZoomMode;
  pageIndex: PageIndex;
  showText?: boolean;
  mediaType: string;
  targetPath: string;
  onToggleTextClick: () => void;
  onZoomChange: (zoomMode: ZoomMode) => void;
  onCloseClick: () => void;
}

export class DetailPageComponent extends React.Component<DetailPageProps, {}> {
  constructor(props) {
    super(props);
  }

  public render() {
    return (
      <div className={style.container}>
        <ToolbarComponent
          zoomMode={this.props.zoomMode}
          onToggleTextClick={this.props.onToggleTextClick}
          onZoomChange={this.props.onZoomChange}
          showToggleMenu={this.props.mediaType === "hocr"}
          onCloseClick={this.props.onCloseClick}
        />
        <HorizontalSeparator className={style.separator} />
        {this.props.mediaType === "hocr" && (
          <HocrProofreaderComponent
            className={style.hocr}
            hocr={this.props.hocr}
            targetWords={this.props.targetWords}
            zoomMode={this.props.zoomMode}
            pageIndex={this.props.pageIndex}
            showText={this.props.showText}
          />
        )}
        {this.props.mediaType === "json" && (
          <JsonReaderComponent
            className={style.json}
            hocr={this.props.hocr}
            targetWords={this.props.targetWords}
          />
        )}
        {this.props.mediaType === "office" && (
          <OfficeReaderComponent targetPath={this.props.targetPath} />
        )}
      </div>
    );
  }
}
