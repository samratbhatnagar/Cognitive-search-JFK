import * as React from "react";
import { Chevron } from "../chevron";
import { cnc } from "../../../util";

const style = require("./json-reader.style.scss");

interface JsonReaderProps {
  className?: string;
  hocr: any;
  targetWords: string[];
}

interface JsonReaderState {
  path: any[];
}

export class JsonReaderComponent extends React.PureComponent<
  JsonReaderProps,
  JsonReaderState
> {
  constructor(props) {
    super(props);

    this.state = {
      path: []
    };
  }

  findNext() {
    console.log("findNext() clicked.");
  }

  findPrevious() {
    console.log("findPrevious() clicked.");
  }

  loadChildren(
    level: number,
    propName: string,
    isArray: boolean = false,
    index: number = 0
  ) {
    console.log("in load children " + propName);
    console.log("at level " + level);

    let newPath = this.state.path.slice(0, level);

    if (isArray) {
      newPath = newPath.concat([index]);
    }

    newPath = newPath.concat([propName]);

    console.log("new path");
    console.log(newPath);

    this.setState({ path: newPath });
  }

  tooltipNeeded(value: string): boolean {
    if (value.length > 17) {
      return true;
    } else {
      return false;
    }
  }

  _renderPanelHeader(count: number) {
    return (
      <div className={style.columnHeader}>
        <label>Top</label>
        <div className={style.columnHeaderMenu}>
          <span className={style.columnHeaderHintText}>1 of {count} found</span>
          <Chevron
            className={cnc(style.smallChevron, style.chevronPrev)}
            onClick={this.findPrevious}
            expanded={false}
          />
          <Chevron
            className={cnc(style.smallChevron, style.chevronNext)}
            onClick={this.findNext}
            expanded={false}
          />
        </div>
      </div>
    );
  }

  _renderPanelItem(
    item: any,
    inArray: boolean,
    index: number,
    levelNum: number
  ) {
    if (!item) {
      console.log("item is null");
      return null;
    }

    let objKeys = Object.keys(item);

    return (
      <li key={index}>
        {objKeys.map((key, idx) => {
          let val = item[key];

          if (typeof val === "object" && item[key]) {
            return (
              <div
                className={cnc(
                  style.lineItemDetail,
                  style.hasChildren,
                  style.selected
                )}
                key={idx}
                onClick={() => this.loadChildren(levelNum, key, inArray, index)}
              >
                <label>{key} (5)</label>
              </div>
            );
          }

          return (
            <div className={style.lineItemDetail} key={idx}>
              <label>{key}</label>
              {this.tooltipNeeded(key) ? (
                <span className={style.simpleTooltip} role="tooltip">
                  {key}
                </span>
              ) : null}
              <span>{item[key]}</span>
            </div>
          );
        })}
      </li>
    );
  }

  _renderPanel(level: any[], levelNum: number = 0) {
    if (!level || level.length === 0 || !level[0]) {
      return null;
    }

    return (
      <div className={style.column} key={levelNum}>
        {this._renderPanelHeader(2)}
        <ul>
          {level.map((item, index) => {
            return this._renderPanelItem(
              item,
              level.length > 1,
              index,
              levelNum
            );
          })}
        </ul>
      </div>
    );
  }

  _renderPath(data: any, path: any[]) {
    console.log("In render path");

    if (!path || path.length === 0 || !data) {
      console.log("no path");
      return null;
    }

    let panels = [];

    for (var i = 0; i < path.length; i++) {
      let pathItem = path[i];

      if (i + 1 < path.length && typeof path[i + 1] === "number") {
        i = i + 1;
        console.log("Item is an array");
      }

      let obj = this._pathReducer(path.slice(0, i + 1), data);

      if (!Array.isArray(obj)) {
        obj = [obj];
      }

      panels.push(this._renderPanel(obj, panels.length + 1));
    }

    console.log("Got panels");
    console.log(panels);

    return panels;
  }

  _pathReducer(path: any[], data: any) {
    return path.reduce((obj, p) => {
      return obj[p];
    }, data);
  }

  public render() {
    let data = JSON.parse(this.props.hocr);
    let path = this.state.path;

    return (
      <div className={style.jsonReaderComponent}>
        <div className={style.jsonRendered}>
          <div className={style.viewport}>
            {this._renderPanel([data])}

            {this._renderPath(data, path)}
          </div>
        </div>
      </div>
    );
  }

  _getData_temp() {
    return {
      rid: "gX9zANivC3VnBgAAAAAAAA==",
      finalText: "Energy drink, VAULT Zero, sugar-free, citrus flavor",
      tags: [
        { name: "energy drink" },
        { name: "vault zero" },
        { name: "sugar-free" },
        { name: "citrus flavor" }
      ],
      version: 1,
      foodGroup: "Beverages",
      nutrients: [
        {
          id: "221",
          description: "Alcohol, ethyl",
          nutritionValue: 0,
          units: "g"
        },
        {
          id: "303",
          description: "Iron, Fe",
          nutritionValue: 0.02,
          units: "mg"
        },
        {
          id: "322",
          description: "Carotene, alpha",
          nutritionValue: 0,
          units: "µg"
        },
        { id: "337", description: "Lycopene", nutritionValue: 0, units: "µg" },
        {
          id: "406",
          description: "Niacin",
          nutritionValue: 0.015,
          units: "mg"
        },
        {
          id: "430",
          description: "Vitamin K (phylloquinone)",
          nutritionValue: 0,
          units: "µg"
        },
        {
          id: "606",
          description: "Fatty acids, total saturated",
          nutritionValue: 0,
          units: "g"
        },
        { id: "614", description: "18:0", nutritionValue: 0, units: "g" },
        {
          id: "630",
          description: "22:1 undifferentiated",
          nutritionValue: 0,
          units: "g"
        },
        { id: "262", description: "Caffeine", nutritionValue: 19, units: "mg" },
        {
          id: "307",
          description: "Sodium, Na",
          nutritionValue: 14,
          units: "mg"
        },
        { id: "309", description: "Zinc, Zn", nutritionValue: 0, units: "mg" },
        {
          id: "318",
          description: "Vitamin A, IU",
          nutritionValue: 0,
          units: "IU"
        },
        { id: "608", description: "6:0", nutritionValue: 0, units: "g" },
        { id: "610", description: "10:0", nutritionValue: 0, units: "g" },
        { id: "627", description: "18:4", nutritionValue: 0, units: "g" },
        {
          id: "645",
          description: "Fatty acids, total monounsaturated",
          nutritionValue: 0,
          units: "g"
        },
        {
          id: "204",
          description: "Total lipid (fat)",
          nutritionValue: 0.08,
          units: "g"
        },
        {
          id: "205",
          description: "Carbohydrate, by difference",
          nutritionValue: 0.7,
          units: "g"
        },
        { id: "207", description: "Ash", nutritionValue: 0.62, units: "g" },
        { id: "255", description: "Water", nutritionValue: 98.35, units: "g" },
        { id: "268", description: "Energy", nutritionValue: 6, units: "kJ" },
        {
          id: "269",
          description: "Sugars, total",
          nutritionValue: 0,
          units: "g"
        },
        {
          id: "306",
          description: "Potassium, K",
          nutritionValue: 3,
          units: "mg"
        },
        {
          id: "323",
          description: "Vitamin E (alpha-tocopherol)",
          nutritionValue: 0,
          units: "mg"
        },
        {
          id: "338",
          description: "Lutein + zeaxanthin",
          nutritionValue: 0,
          units: "µg"
        },
        {
          id: "417",
          description: "Folate, total",
          nutritionValue: 0,
          units: "µg"
        },
        {
          id: "617",
          description: "18:1 undifferentiated",
          nutritionValue: 0,
          units: "g"
        },
        {
          id: "626",
          description: "16:1 undifferentiated",
          nutritionValue: 0,
          units: "g"
        },
        {
          id: "263",
          description: "Theobromine",
          nutritionValue: 0,
          units: "mg"
        },
        {
          id: "321",
          description: "Carotene, beta",
          nutritionValue: 0,
          units: "µg"
        },
        {
          id: "405",
          description: "Riboflavin",
          nutritionValue: 0,
          units: "mg"
        },
        {
          id: "573",
          description: "Vitamin E, added",
          nutritionValue: 0,
          units: "mg"
        },
        { id: "613", description: "16:0", nutritionValue: 0, units: "g" },
        {
          id: "631",
          description: "22:5 n-3 (DPA)",
          nutritionValue: 0,
          units: "g"
        },
        {
          id: "291",
          description: "Fiber, total dietary",
          nutritionValue: 0,
          units: "g"
        },
        {
          id: "301",
          description: "Calcium, Ca",
          nutritionValue: 2,
          units: "mg"
        },
        {
          id: "312",
          description: "Copper, Cu",
          nutritionValue: 0.005,
          units: "mg"
        },
        {
          id: "328",
          description: "Vitamin D (D2 + D3)",
          nutritionValue: 0,
          units: "µg"
        },
        {
          id: "415",
          description: "Vitamin B-6",
          nutritionValue: 0,
          units: "mg"
        },
        {
          id: "431",
          description: "Folic acid",
          nutritionValue: 0,
          units: "µg"
        },
        {
          id: "578",
          description: "Vitamin B-12, added",
          nutritionValue: 0,
          units: "µg"
        },
        { id: "612", description: "14:0", nutritionValue: 0, units: "g" },
        {
          id: "629",
          description: "20:5 n-3 (EPA)",
          nutritionValue: 0,
          units: "g"
        },
        {
          id: "317",
          description: "Selenium, Se",
          nutritionValue: 0.2,
          units: "µg"
        },
        {
          id: "334",
          description: "Cryptoxanthin, beta",
          nutritionValue: 0,
          units: "µg"
        },
        { id: "607", description: "4:0", nutritionValue: 0, units: "g" },
        { id: "609", description: "8:0", nutritionValue: 0, units: "g" },
        { id: "611", description: "12:0", nutritionValue: 0, units: "g" },
        { id: "628", description: "20:1", nutritionValue: 0, units: "g" },
        {
          id: "646",
          description: "Fatty acids, total polyunsaturated",
          nutritionValue: 0,
          units: "g"
        },
        { id: "203", description: "Protein", nutritionValue: 0.25, units: "g" },
        { id: "208", description: "Energy", nutritionValue: 1, units: "kcal" },
        {
          id: "304",
          description: "Magnesium, Mg",
          nutritionValue: 3,
          units: "mg"
        },
        {
          id: "305",
          description: "Phosphorus, P",
          nutritionValue: 0,
          units: "mg"
        },
        {
          id: "401",
          description: "Vitamin C, total ascorbic acid",
          nutritionValue: 0,
          units: "mg"
        },
        {
          id: "418",
          description: "Vitamin B-12",
          nutritionValue: 0,
          units: "µg"
        },
        {
          id: "435",
          description: "Folate, DFE",
          nutritionValue: 0,
          units: "µg"
        },
        {
          id: "601",
          description: "Cholesterol",
          nutritionValue: 0,
          units: "mg"
        },
        {
          id: "618",
          description: "18:2 undifferentiated",
          nutritionValue: 0,
          units: "g"
        },
        {
          id: "619",
          description: "18:3 undifferentiated",
          nutritionValue: 0,
          units: "g"
        },
        {
          id: "620",
          description: "20:4 undifferentiated",
          nutritionValue: 0,
          units: "g"
        },
        { id: "319", description: "Retinol", nutritionValue: 0, units: "µg" },
        {
          id: "320",
          description: "Vitamin A, RAE",
          nutritionValue: 0,
          units: "µg"
        },
        { id: "324", description: "Vitamin D", nutritionValue: 0, units: "IU" },
        {
          id: "404",
          description: "Thiamin",
          nutritionValue: 0.025,
          units: "mg"
        },
        {
          id: "421",
          description: "Choline, total",
          nutritionValue: 0.3,
          units: "mg"
        },
        {
          id: "432",
          description: "Folate, food",
          nutritionValue: 0,
          units: "µg"
        },
        {
          id: "621",
          description: "22:6 n-3 (DHA)",
          nutritionValue: 0,
          units: "g"
        }
      ],
      servings: [
        { amount: 1, description: "serving (8 fl oz)", weightInGrams: 246 },
        { amount: 12, description: "fl oz", weightInGrams: 360 }
      ],
      _ts: 1560469821,
      isFromSurvey: false,
      manufacturerName: "The Coca-Cola Company",
      commonName: null,
      id: "14641"
    };
  }
}
