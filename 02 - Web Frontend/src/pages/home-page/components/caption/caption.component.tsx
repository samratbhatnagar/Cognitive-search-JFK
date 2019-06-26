import * as React from "react";
import { LogoComponent } from "../../../../common/components/logo";

const style = require("./caption.style.scss");

export const CaptionComponent = () => (
  <LogoComponent
    classes={{ container: style.logoContainer, img: style.logoImg }}
  />
);
