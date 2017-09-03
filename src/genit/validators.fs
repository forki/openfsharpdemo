module validators

open System
open System.Net.Mail
open System.Text.RegularExpressions

let validate_required property value =
  if String.IsNullOrWhiteSpace value
  then Some (property, sprintf "%s is required" property)
  else None

let validate_required_dropdown property value =
  if String.IsNullOrWhiteSpace value || value = "0"
  then Some (property, sprintf "%s is required" property)
  else None

let validate_equal property1 property2 value1 value2  =
  if value1 <> value2
  then Some (property2, sprintf "%s must be the same as %s" property2 property1)
  else None

let validate_email property value =
  let isEmail = try MailAddress(value)|> ignore; true with | _ -> false
  if not isEmail
  then Some (property, sprintf "%s is not a valid email" property)
  else None

let validate_phone property (value : string) =
  if value.Length > 15
  then Some (property, sprintf "%s can not be more than 15 characters" property)
  else None

let private passwordPattern = @"(\w){6,100}"
let validate_password property value =
  if value = null || not <| Regex(passwordPattern).IsMatch(value)
  then Some (property, sprintf "%s must be between 6 and 100 characters" property)
  else None

let validate_integer property value =
  let parsed, _ = System.Int32.TryParse(value)
  if not parsed
  then Some (property, sprintf "%s is not a valid number (int)" property)
  else None

let validate_double property value =
  let parsed, _ = System.Double.TryParse(value)
  if not parsed
  then Some (property, sprintf "%s is not a valid number (decimal)" property)
  else None

let validate_bool property value =
  let parsed, _ = System.Boolean.TryParse(value)
  if not parsed
  then Some (property, sprintf "%s is not a valid bool" property)
  else None

let validate_datetime property value =
  let parsed, _ = System.DateTime.TryParse(value)
  if not parsed
  then Some (property, sprintf "%s is not a valid date" property)
  else None

let validate_min property value min =
  let parsed, value = System.Int32.TryParse(value)
  if not parsed //if we can't parse its not an error because it may not be required
  then None
  else if value < min
  then Some (property, sprintf "%s can not be below %i" property min)
  else None

let validate_max property value max =
  let parsed, value = System.Int32.TryParse(value)
  if not parsed //if we can't parse its not an error because it may not be required
  then None
  else if value > max
  then Some (property, sprintf "%s can not be above %i" property max)
  else None

let validate_range property value min max =
  let parsed, value = System.Int32.TryParse(value)
  if not parsed //if we can't parse its not an error because it may not be required
  then None
  else if value < min || value > max
  then Some (property, sprintf "%s must be between %i and %i" property min max)
  else None

let validate_reference _ _ _ _ = //page property value required =
  //todo?
  None
