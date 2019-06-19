import * as React from "react";
const styles = require("./style.scss");

interface Props {
  className: string;
}

export const LogoImg: React.StatelessComponent<Props> = props => (
  <div className={`${styles.logo} ${props.className}`} />
);
