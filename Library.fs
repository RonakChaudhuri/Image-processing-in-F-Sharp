//
// F# image processing functions.
//
// More details?
// This program does various image operations of ppm image files such 
// as grayscale, threshold, FlipHorizontal, Edge Detect, and Right
// Rotation 90 degrees
//
// Name? School? Date?
// Ronak Chaudhuri, University of Illinois at Chicago, March 30th 2023

namespace ImageLibrary

module Operations =
  //
  // all functions must be indented
  //

  //
  // Grayscale:
  //
  // Converts the image into grayscale and returns the 
  // resulting image as a list of lists. Pixels in grayscale
  // have the same value for each of the Red, Green and Blue
  // values in the RGB value.  Conversion to grayscale is done
  // by using a WEIGHTED AVERAGE calculation.  A normal average
  // (adding the three values and dividing by 3) is not the best,
  // since the human eye does not perceive the brightness of 
  // red, green and blue the same.  The human eye perceives 
  // green as brighter than red and it perceived red as brighter
  // than blue.  Research has shown that the following weighted
  // values should be used when calculating grayscale.
  //  - the green value should account for 58.7% of the grayscale.
  //  - the red value should account for   29.9% of the grayscale.
  //  - the blue value should account for  11.4% of the grayscale.
  //
  // So if the RGB values were (25, 75, 250), the grayscale amount 
  // would be 80, (25 * 0.299 + 75 * 0.587 + 250 * 0.114 => 80)
  // and then all three RGB values would become 80 or (80, 80, 80).
  // We will use truncation to cast from the floating point result 
  // to the integer grayscale value.
  //
  // Returns: updated image.
  //
  let grayVal (r,g,b) = int(float r * 0.299 + float g * 0.587 + float b * 0.114) |> fun x -> (x,x,x) 
  //Calculates grayscale average value and takes that value
  //then returns a list with 3 of those values
  let rec Grayscale (width:int) 
                    (height:int) 
                    (depth:int) 
                    (image:(int*int*int) list list) = 
    List.map (fun x -> List.map grayVal x) image
    //Applies the computation of turning each (r,g,b) list of image
    //into a list of grayscale average list using the grayVal helper
    //function so every element of image is grayscaled. 


  //
  // Threshold
  //
  // Thresholding increases image separation --- dark values 
  // become darker and light values become lighter. Given a 
  // threshold value in the range 0 < threshold < color depth,
  // each RGB value is compared to see if it's > threshold.
  // If so, that RGB value is replaced by the color depth;
  // if not, that RGB value is replaced with 0. 
  //
  // Example: if threshold is 100 and depth is 255, then given 
  // a pixel (80, 120, 160), the new pixel is (0, 255, 255).
  //
  // Returns: updated image.
  // 
  let applyThreshold t d (r, g, b) =
    match r <= t, g <= t, b <= t with
    | true, false, false -> (0, d, d)
    | false, true, false -> (d, 0, d)
    | false, false, true -> (d, d, 0)
    | true, false, true -> (0, d, 0)
    | true, true, false -> (0, 0, d)
    | false, true, true -> (d, 0, 0)
    | true, true, true -> (0, 0, 0)
    | false, false, false -> (d, d, d)
  //This function applies the threshold for a list of (r,g,b) values based
  //on t threshold and d depth input parameters.
  //It looks at every case and for each individual r,g,b value it
  //replaces it with 0 if <= to t and d if greater than t
  let rec Threshold (width:int) 
                    (height:int)
                    (depth:int)
                    (image:(int*int*int) list list)
                    (threshold:int) = 
    List.map (fun x -> List.map (applyThreshold threshold depth) x) image
    //For every (r,g,b) list element of image, this function will apply
    //the mapping of turning each (r,g,b) to it's appropriate threshold
    //value using the applyThreshold helper function 


  
  //
  // FlipHorizontal:
  //
  // Flips an image so that what’s on the left is now on 
  // the right, and what’s on the right is now on the left. 
  // That is, the pixel that is on the far left end of the
  // row ends up on the far right of the row, and the pixel
  // on the far right ends up on the far left. This is 
  // repeated as you move inwards toward the row's center.
  //
  // Returns: updated image.
  //
  //let flipList list = List.rev list
  let rec FlipHorizontal (width:int)
                         (height:int)
                         (depth:int)
                         (image:(int*int*int) list list) = 
    List.map List.rev image
    //Applies list reverse function to each element(list) in image
    //so every list in image is reversed to produce of horizontal image


  //
  // Edge Detection:
  //
  // Edge detection is an algorithm used in computer vision to help
  // distinguish different objects in a picture or to distinguish an
  // object in the foreground of the picture from the background.
  //
  // Edge Detection replaces each pixel in the original image with
  // a black pixel, (0, 0, 0), if the original pixel contains an 
  // "edge" in the original image.  If the original pixel does not
  // contain an edge, the pixel is replaced with a white pixel 
  // (255, 255, 255).
  //
  // An edge occurs when the color of pixel is "significantly different"
  // when compared to the color of two of its neighboring pixels. 
  // We only compare each pixel in the image with the 
  // pixel immediately to the right of it and with the pixel
  // immediately below it. If either pixel has a color difference
  // greater than a given threshold, then it is "significantly
  // different" and an edge occurs. Note that the right-most column
  // of pixels and the bottom-most column of pixels can not perform
  // this calculation so the final image contain one less column
  // and one less row than the original image.
  //
  // To calculate the "color difference" between two pixels, we
  // treat the each pixel as a point on a 3-dimensional grid and
  // we calculate the distance between the two points using the
  // 3-dimensional extension to the Pythagorean Theorem.
  // Distance between (x1, y1, z1) and (x2, y2, z2) is
  //  sqrt ( (x1-x2)^2 + (y1-y2)^2 + (z1-z2)^2 )
  //
  // The threshold amount will need to be given, which is an 
  // integer 0 < threshold < 255.  If the color distance between
  // the original pixel either of the two neighboring pixels 
  // is greater than the threshold amount, an edge occurs and 
  // a black pixel is put in the resulting image at the location
  // of the original pixel. 
  //
  // Returns: updated image.
  //
  let colorDifference (c1:int*int*int) (c2:int*int*int) =
    let (r1, g1, b1) = c1
    let (r2, g2, b2) = c2
    sqrt(float ((r1-r2)*(r1-r2) + (g1-g2)*(g1-g2) + (b1-b2)*(b1-b2)))
    //This function takes in two tuples of rgb value lists, and then
    //calculates the color difference between the two pixels by
    //calculating the difference between the two points
      
  let rec updateList (curr:int*int*int) (right:int*int*int) (bottom:int*int*int) t =
    let (r1, g1, b1) = curr
    let (r2, g2, b2) = right
    let (r3, g3, b3) = bottom
    let colorDiff1 = colorDifference curr right
    let colorDiff2 = colorDifference curr bottom
    if colorDiff1 > float t || colorDiff2 > float t then
        (0, 0, 0)
    else
        (255, 255, 255)
    //This function takes in three rgb list pixels, the current one,
    //the right neighbor and the left neighbor, and a threshold value.
    //It calculates the color difference between the current pixels
    //and the right and bottoms ones if any of the color differences
    //are greater than the threshold it returns a black pixel, if not
    //then it returns a white pixel
        
  let update (row:(int*int*int) list) (rowIndex:int) image t=
        List.init (List.length row - 1) (fun x ->
            let hd = List.nth row x
            let right = List.nth row (x+1)
            let bottom = List.nth (List.nth image (rowIndex + 1)) x
            updateList hd right bottom t)
        //This function takes in a list of pixels, an index for the row, 
        //the image and a threshold value, and it creates a new list,
        //updating the list in image with the updateList function to
        //match the edge detect output
        
  let rec EdgeDetect (width:int)
               (height:int)
               (depth:int)
               (image:(int*int*int) list list)
               (threshold:int) = 
    List.init (height - 1) (fun x -> update (List.nth image x) x image threshold)
    //Applies the updating of each row of pixels to every row in image
    //and initialized a new list of tuples with all the updated pixels,
    //creating the new list


  //
  // RotateRight90:
  //
  // Rotates the image to the right 90 degrees.
  //
  // Returns: updated image.
  //
  let rec RotateRight90 (width:int)
                        (height:int)
                        (depth:int)
                        (image:(int*int*int) list list) = 
    List.map List.rev (List.transpose image)
    //This function applies the List.rev to the tranpose of image,
    //List.tranpose switches rows and cols, and rev reverses the list 
    //to rotates the image 90 degrees

