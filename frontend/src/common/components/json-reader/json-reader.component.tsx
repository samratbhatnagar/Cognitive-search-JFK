import * as React from "react";

const style = require("./json-reader.style.scss");

const jsonRenderedWidth = {
  // width: "1200px"
};

interface JsonReaderProps {
  className?: string;
  hocr: string;
  targetWords: string[];
}

export class JsonReaderComponent extends React.PureComponent<JsonReaderProps> {
  constructor(props) {
    super(props);
  }

  public render() {
    return (
      <div className={`${style.jsonReaderComponent}`}>
        <div className={`${style.jsonRendered}`}>
          <div className={`${style.viewport}`}>
            <ul style={jsonRenderedWidth}>
              <li>
                <div className={`${style.lineItemDetail}`}>
                  <label>Address</label>
                  <span>1234 Main St., City, ST 12345</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Phone</label>
                  <span>000-123-4567</span>
                </div>
                <div
                  className={`${style.lineItemDetail} ${style.hasChildren} ${
                    style.selected
                  }`}
                >
                  <label>Employees (5)</label>
                </div>
                <div className={`${style.lineItemDetail} ${style.hasChildren}`}>
                  <label>Contractors (12)</label>
                </div>
              </li>
            </ul>

            <ul>
              <li>
                <div className={`${style.lineItemDetail}`}>
                  <label>First Name</label>
                  <span>Joseph</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Last Name</label>
                  <span>Johnson</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Position</label>
                  <span>Owner, CEO</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Biography</label>
                  <span>
                    Lorem Ipsum has been the company's standard dummy text ever
                    since the 1500s, when an unknown printer took a galley of
                    type and scrambled it to make a type specimen book.
                  </span>
                </div>
                <div
                  className={`${style.lineItemDetail} ${style.hasChildren} ${
                    style.selected
                  }`}
                >
                  <label>Contact Info (3)</label>
                </div>
              </li>

              <li>
                <div className={`${style.lineItemDetail}`}>
                  <label>First Name</label>
                  <span>Molly</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Last Name</label>
                  <span>Reynolds</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Position</label>
                  <span>CFO</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Biography</label>
                  <span>
                    Lorem Ipsum has been the company's standard dummy text ever
                    since the 1500s, when an unknown printer took a galley of
                    type and scrambled it to make a type specimen book.
                  </span>
                </div>
                <div className={`${style.lineItemDetail} ${style.hasChildren}`}>
                  <label>Contact Info (3)</label>
                </div>
              </li>

              <li>
                <div className={`${style.lineItemDetail}`}>
                  <label>First Name</label>
                  <span>Steve</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Last Name</label>
                  <span>Sanders</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Position</label>
                  <span />
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Biography</label>
                  <span>
                    Aliquam aliquam turpis id elit egestas, id malesuada tellus
                    vestibulum. Proin nibh urna, efficitur eu
                    auctorpellentesque.
                  </span>
                </div>
                <div className={`${style.lineItemDetail} ${style.hasChildren}`}>
                  <label>Contact Info (3)</label>
                </div>
              </li>

              <li>
                <div className={`${style.lineItemDetail}`}>
                  <label>First Name</label>
                  <span>Tina</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Last Name</label>
                  <span>Tallworth</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Position</label>
                  <span>Company Manager</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Biography</label>
                  <span>
                    Aliquam aliquam turpis id elit egestas, id malesuada tellus
                    vestibulum. Proin nibh urna, efficitur eu
                    auctorpellentesque.
                  </span>
                </div>
                <div className={`${style.lineItemDetail} ${style.hasChildren}`}>
                  <label>Contact Info (3)</label>
                </div>
              </li>
            </ul>

            <ul>
              <li>
                <div className={`${style.lineItemDetail}`}>
                  <label>Phone</label>
                  <span>111-111-1111</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Email</label>
                  <span>j-johnson@abc-company.com</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Twitter</label>
                  <span>@jjohnson</span>
                </div>
                <div
                  className={`${style.lineItemDetail} ${style.hasChildren} ${
                    style.selected
                  }`}
                >
                  <label>Extra Layer (2)</label>
                </div>
              </li>
            </ul>

            <ul>
              <li>
                <div className={`${style.lineItemDetail}`}>
                  <label>Key</label>
                  <span>Value</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Key 2</label>
                  <span>
                    Lorem ipsum dolor sit amet, consect etur adipiscing elit.
                  </span>
                </div>
                <div
                  className={`${style.lineItemDetail} ${style.hasChildren} ${
                    style.selected
                  }`}
                >
                  <label>Extra, Extra Layer (2)</label>
                </div>
              </li>
            </ul>

            <ul>
              <li>
                <div className={`${style.lineItemDetail}`}>
                  <label>Key</label>
                  <span>Value</span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Key 2</label>
                  <span>
                    Lorem ipsum dolor sit amet, consect etur adipiscing elit.
                  </span>
                </div>
                <div className={`${style.lineItemDetail}`}>
                  <label>Key 3</label>
                  <span>Value</span>
                </div>
              </li>
            </ul>
          </div>
        </div>
      </div>
    );
  }
}
